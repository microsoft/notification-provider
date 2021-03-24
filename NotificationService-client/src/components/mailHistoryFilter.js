// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { ActionButton,OverflowSet, Callout,PrimaryButton,Dropdown,DefaultButton,unregisterIcons,
  registerIcons, FontWeights, mergeStyleSets, DirectionalHint, Stack, 
  TextField, MessageBar, MessageBarButton} from 'office-ui-fabric-react';
import { useId } from '@uifabric/react-hooks';
unregisterIcons([
    'Info'
    ]);
registerIcons({
    icons: {
      'Info': (
        <></>)
    }
  });

const styles = mergeStyleSets({
  messageBar : {
    minWidth:130
  },
  buttonArea: {
    minWidth: 130,
    height: 32,
  },
  callout: {
    maxWidth: 300,
    minWidth: 200,
  },
  header: {
    padding: '20',
  },
  title: [
    {
      margin: 0,
      fontWeight: FontWeights.semilight,
    },
  ],
  inner: {
    height: '100%',
    padding: '0px 10px 10px',
  },
  actions: {
    position: 'relative',
    width: '100%',
    whiteSpace: 'nowrap',
  },
  subtext: [
    {
      margin: 0,
      fontWeight: FontWeights.semilight,
    },
  ]
});

export default function  MailHistoryFilter (props) {
  const [isCalloutVisible, setIsCalloutVisible] = useState(false);
  const [filterErrorMessage, setFilterErrorMessage] = useState(undefined);
  const [opErrorMessage, setOpErrorMessage] = useState(undefined);
  const [inputBoxErrorMessage, setInputBoxErrorMessage] = useState(undefined);
  const [comboboxErrorMessage, setComboboxErrorMessage] = useState(undefined);
  const [isMessageCalloutVisible, setIsMessageCalloutVisible] = useState(false);
  const [calloutTarget, setCalloutTarget] = useState();
  const [calloutSelected, setCalloutSelected] = useState();
  const [filterCount, setFilterCount] = useState([]);
  const [selectedFilter, setSelectedFilter] = useState(undefined);
  const [selectedOp, setSelectedOp] = useState(undefined);
  const [selectedVal,setSelectedVal] = useState();
  const [selectedComboValue, setSelectedComboValue] = useState([]);
  const [filterVal, setFilterVal] = useState([]);

  const labelId = useId('callout-label');
  const descriptionId = useId('callout-description');
  const createMessageBar = (e) =>{
      if(filterCount.length<options.length && selectedFilter.key!==undefined) {
        setFilterCount(filterCount.concat(selectedFilter.key));
      }
      setIsCalloutVisible(false);
      
  }
  const reset = ()=> {
    setSelectedComboValue([]);
    setSelectedVal(undefined);
    setSelectedOp(undefined);
  }
  const resetErrorMessage =()=>{
    setOpErrorMessage(undefined);
    setFilterErrorMessage(undefined);
    setInputBoxErrorMessage(undefined);
    setComboboxErrorMessage(undefined);
  }
  const toggleIsCalloutVisible = (i)=> {
        reset();
        resetErrorMessage();
        setSelectedFilter(undefined);
        setIsCalloutVisible(!isCalloutVisible);
  }
  const toggIsMessageCalloutVisible = (i)=>{
      reset();
      setCalloutSelected(i);
      setCalloutTarget(`.messagebar${i}`);
      const selectedFilterValues = filterVal.filter(e=>e.key === i);
      const selectedOperator = props.operatorItems.filter(e=>e.key ===selectedFilterValues[0].op);
      setSelectedOp(selectedOperator[0]);
      if(selectedFilterValues[0].selector==="InputBox")
        setSelectedVal(selectedFilterValues[0].value);
      else {
        setSelectedComboValue(selectedFilterValues[0].value);
      }
      setIsMessageCalloutVisible(!isMessageCalloutVisible);
  }
  const dismissMessageBar = (i)=> {
    setIsMessageCalloutVisible(false);
    const temp = [...filterCount];
    temp.splice(i, 1);
    setFilterCount(temp);
    const temp1 = [...filterVal];
    temp1.splice(i,1);
    setFilterVal(temp1);
  }
  const options = props.filterItems;
  const filterOnChange = (e,i)=> {
      resetErrorMessage();
      setSelectedFilter(i);
  }
  const operatorOnChange = (e,i)=>{
      resetErrorMessage();
      setSelectedOp(i);

  }
  const inputboxOnChange = (e,i)=>{
      setSelectedVal(selectedVal => (selectedVal, i.trim()));
  }
  const comboboxOnChange = (e, i)=>{
      if (i) {
        setSelectedComboValue(
          i.selected ? [...selectedComboValue, i.key] : selectedComboValue.filter(element => element !== i.key));
      }
  }
  const onResetFilter = ()=>{
     setIsCalloutVisible(false);
     setIsMessageCalloutVisible(false);
     const temp1 = [...filterVal]
     temp1.splice(0,temp1.length);
     setFilterVal(temp1);
     const temp = [...filterCount];
     temp.splice(0,temp.length);
     setFilterCount(temp);
     props.onResetFilter();
  }
  const validateOnSelectAddFilter = ()=> {
    if(selectedFilter===undefined) {
        setFilterErrorMessage("Select a Filter");
        return false;
    }
    if(selectedOp===undefined) {
        setOpErrorMessage("Select an Operator");
        return false;
    }
    if(selectedFilter.selector === 'InputBox'){
      if(selectedVal === undefined){
        setInputBoxErrorMessage("Enter value");
        return false;
      }
    }
    if(selectedFilter.selector === 'ComboBox'){
      if(selectedComboValue.length === 0){
        setComboboxErrorMessage("Select value(s)");
        return false;
      }
    }
    return true;
  }
  const onApplyFilter = () => {
    props.onApplyFilter(filterVal)
  };
  const onSelectAddFilter = (o) => {
      if (validateOnSelectAddFilter()) {
        var filter = {
          key:selectedFilter.key,
          text:selectedFilter.text,
          op : selectedOp.key,
          value : selectedFilter.selector==="InputBox"? selectedFilter.isList ? selectedVal?.split(",") : selectedVal : selectedComboValue,
          selector : selectedFilter.selector

      }
      setFilterVal(filterVal=>[...filterVal, filter]);
      props.onSelectAddFilter(o);
      createMessageBar();
      }
      else return;
    };
  const updateFilter = (o) => { //update the filter whose key === o
    const currentFilter = [...filterVal];
    const updatedFilter = currentFilter.map(item => 
      item.key === o
      ? (item.selector === "InputBox" ?{...item, 'value' : selectedVal} :{...item, 'value' : selectedComboValue}  )
      : item );
    setFilterVal(updatedFilter);

  }
  const onSelectFilter = (o) => {
      setSelectedFilter(o);
      if (validateOnSelectAddFilter()) {
       updateFilter(o); //update filter whose key == o
       props.onSelectFilter(o, filterVal.filter(element=>element.key === o));
       toggIsMessageCalloutVisible(calloutSelected);
      } 
      else return;
    };
  const onFilterDismiss = (o) => {
    props.onFilterDismiss(o); 
    dismissMessageBar(o);
    } ;
  
  const getDynamicFilters = () => {
    return (filterCount.map((i,indx) => ( {
      key : i,
      onRender : () => { 
      const count = Array.isArray(filterVal.filter(element=>element.key===i)[0].value) 
      ? filterVal.filter(element=>element.key===i)[0].value.length
      : filterVal.filter(element=>element.key===i)[0].value?.split(",").length;
      return (
      <div key={i} id = {i} style={{padding : 5,  height:"32px"}}>
          <div key={i} id = {i} className = {`messagebar${i}`}>
              <MessageBar key={i}
                  styles={{
                      root: {
                      background: 'rgba(113, 175, 229, 0.2)',
                      color: '#00188f',
                     
                      },
                      icon: {
                          
                      color: '#00188f'
                      }
                  }}
                  onDismiss={()=>{onFilterDismiss(indx)}}
                  dismissButtonAriaLabel="Close">
                  <MessageBarButton onClick={() => toggIsMessageCalloutVisible(i)}>
                      {options[i].text} ({count?count:0})
                  </MessageBarButton>
              </MessageBar>
          </div> 
      </div>) }})));
  }
  const onRenderOverFlowSetItem = (item) => {
      if (item.onRender) {
        return item.onRender(item);
      }
  }

  return ( 
    <Stack horizontal > 
         <OverflowSet items = {getDynamicFilters()} overflowItems={getDynamicFilters()}
         onRenderItem = {onRenderOverFlowSetItem} onRenderOverflowButton={onRenderOverFlowSetItem}/>
        {isMessageCalloutVisible && (
        <Callout
            directionalHint={DirectionalHint.topAutoEdge}
            key={calloutSelected}
            className={styles.callout}
            ariaLabelledBy={labelId}
            ariaDescribedBy={descriptionId}
            role="alertdialog"
            gapSpace={0}
            target= {calloutTarget}
            onDismiss={() => toggIsMessageCalloutVisible(calloutSelected)}>
            <div className ={styles.inner}> 
                <Dropdown label="Operator" 
                selectedKey = {selectedOp?.key}
                options={props.operatorItems}onChange={operatorOnChange}
                errorMessage={opErrorMessage}/>
                {options[calloutSelected].selector==="InputBox"
                ?<TextField label="Value" value = {selectedVal} onChange = {inputboxOnChange} errorMessage={inputBoxErrorMessage}/> 
                :<Dropdown multiSelect label="Value"
                  
                 selectedKeys = {selectedComboValue}
                 options={options[calloutSelected].value}  onChange = {comboboxOnChange}
                 errorMessage={comboboxErrorMessage}/>}
            </div> 
            <div className={styles.actions}>
                <Stack horizontal horizontalAlign="center" verticalAlign="center" tokens={{childrenGap:30, padding:5}}>
                    <PrimaryButton  onClick = {()=>onSelectFilter(calloutSelected)} text="Select" />
                </Stack>
            </div>
        </Callout>
    )}
    <span className={styles.buttonArea}>
        <ActionButton onClick={toggleIsCalloutVisible} text= 'Add Filter' iconProps={{iconName:'Filter'}} />
    </span>
    <span style={{padding:5}}>
        <PrimaryButton text= 'Apply' iconProps={{iconName:'CheckMark'}} onClick={onApplyFilter}/>
    </span>
    <span style={{padding:5}}>
        <DefaultButton text= 'Reset' iconProps={{iconName:'CalculatorMultiply'}} onClick={onResetFilter} />
    </span>
    {isCalloutVisible && (
        <Callout
          className={styles.callout}
          ariaLabelledBy={labelId}
          ariaDescribedBy={descriptionId}
          role="alertdialog"
          gapSpace={0}
          target={`.${styles.buttonArea}`}
          onDismiss={toggleIsCalloutVisible}>
              <div className={styles.inner}>
                <Dropdown 
                label="Filter" 
                placeholder="Select Filter" 
                options={options.filter(e =>  !filterVal.some(o => o.key === e.key))}
                //selectedKey={selectedFilter==undefined?undefined:selectedFilter.key}
                onChange={filterOnChange}
                errorMessage={filterErrorMessage}
                /> 
                <Dropdown label="Operator" 
                placeholder="Select Operator" 
                options={props.operatorItems}
                onChange={operatorOnChange}
                errorMessage={opErrorMessage}/>
                {selectedFilter?.key!==undefined
                    ?(options[selectedFilter.key].selector==="InputBox"
                    ?<TextField placeholder={options[selectedFilter.key].placeholder} label="Value"
                     onChange = {inputboxOnChange} errorMessage={inputBoxErrorMessage}/>
                    :<Dropdown placeholder="Select Values" multiSelect label="Value" 
                    selectedKeys = {selectedComboValue} 
                    options={options[selectedFilter.key].value} 
                    onChange = {comboboxOnChange}
                    errorMessage={comboboxErrorMessage}/>)
                    :''
                } </div><div className={styles.actions}>
                <Stack horizontal horizontalAlign="center" verticalAlign="center" tokens={{childrenGap:15, padding:5}}>
                <PrimaryButton  onClick = {onSelectAddFilter} text="Select" />
                <DefaultButton  onClick = {toggleIsCalloutVisible} text="Cancel" /> 
                </Stack>
                </div>
        </Callout>
      )}
      
    </Stack>
  );
};
