// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { useBoolean } from '@uifabric/react-hooks';
import {
    SelectionMode, ShimmeredDetailsList, ConstrainMode, Link, ScrollablePane, ScrollbarVisibility, ActionButton,
    Sticky, StickyPositionType, Selection, mergeStyleSets, Stack
} from 'office-ui-fabric-react';
import { getMeetingHistory, getMeetingBody, getApplications } from "../services";
import ResendModal from './resendModal';
import { CoherencePagination } from '@cseo/controls';
import ViewMailModal from './viewMailModal';
import MailHistoryFilter from './mailHistoryFilter';
import {_Styles} from './PageStyles';
import {copyToClipboard} from '../utils';
import {ToastContainer} from 'react-toastify';
import {AppConstants} from './constants';
import 'react-toastify/dist/ReactToastify.css';

const DEFAULT_PAGE_SIZE = 100;
const TOTAL_RECORDS = 10000;
export default function MeetingHistory(propertis) {

    const [hideDialog, { toggle: toggleHideDialog }] = useBoolean(true);

    const [mailDialog, setMailDialog] = useState(true);

    const [showShimmer, setShowShimmer] = useState(true);

    const [meetingHistoryData, setMeetingHistoryData] = useState([]);

    const [nextRecord, setNextRecord] = useState([]);

    const [selectedItem, setSelectedItem] = useState([]);

    const [activeItem, setActiveItem] = useState();

    const [filter, setFilter] = useState([]);

    const [mailBody, setMailBody] = useState();

    const [loader, setLoader] = useState(true);

    const pageCount = React.useRef(0);

    const selectedPage = React.useRef(1);

    const defaultPageSize = React.useRef(DEFAULT_PAGE_SIZE);

    const [disableResend, setDisableResend] = useState(true);

    const [filterProps, setFilterProps] = useState([]);

    const [applicationName, setApplicationName] = useState("");

    let applications = [];

    const onPageChange = (newPageNumber) => {
        if (newPageNumber !== selectedPage.current) {
            selectedPage.current = newPageNumber;
            const token = selectedPage.current > 1 ? nextRecord[selectedPage.current - 2] : null;
            fetchMeetingHistory(filter, token, null);
        }
    };

    const paginationProps = {
        pageCount: pageCount.current,
        selectedPage: selectedPage.current,
        previousPageAriaLabel: 'previous page',
        nextPageAriaLabel: 'next page',
        inputFieldAriaLabel: 'page number',
        
        onPageChange: onPageChange
    };

    useEffect(() => {
        fetchApplicationNames();
    },[]);

    const fetchApplicationNames = () => {
        getApplications().then(res => {
            var apps = res?.data.map((o,i)=> {return {key:o, text: o};});
            filterProperties.push({ key: 4, text: "Application", selector:"ComboBox", value: apps, placeholder: AppConstants.PlaceholderSelectValue, isList: true})
            setFilterProps(filterProperties);
            applications = res?.data;
            setApplicationName(applications?.[0]);
            fetchMeetingHistory(null, null, null);
          }).catch((error) =>  {
              console.log("aplication fetch error:  " + error)
        });
    }

    const fetchMeetingHistory = (filter, token) => {
        setShowShimmer(true);
        getMeetingHistory(token, filter).then(res => {
            setShowShimmer(false);
            setMeetingHistoryData(res.data);
            nextRecord.push({
                "nextPartitionKey": res.headers['x-nextpartitionkey'],
                "nextRowKey": res.headers['x-nextrowkey']
            });
            setNextRecord(nextRecord);
        }
        ).catch(() => { });
        pageCount.current = Math.ceil(TOTAL_RECORDS / defaultPageSize.current);
    }
    const classNames = mergeStyleSets({
        header: {
            margin: 0,
            backgroundColor: 'white',
        }
    });

    const selection = new Selection({
        onSelectionChanged: () => {
            setSelectedItem(selection.getSelection());
            if(selection.count > 0){
                setDisableResend(false);
            }
            else {
                setDisableResend(true);
            }
        }
    });

    const columnNames = ["ID", "Application", "Subject", "Required Attendees", "From", "Status", "SentOn", "Error"];

    const fieldNames = ["notificationId", "application", "subject", "requiredAttendees", "from", "status", "sendOnUtcDate", "errorMessage"];

    const columns = columnNames.map((item, index) => (
        {
            key: 'column' + (index + 1),
            name: item,
            fieldName: fieldNames[index],
            minWidth: 10,
            maxWidth: 120,
            isResizable: true,
            isHeader:true

        }));

    const overflowCol = {
        key: 'column',
        name: "Action",
        fieldName: "Action",
        minWidth: 50,
        maxWidth: 80,
        isResizable: true,
        onRender: () => {
            return <Link
                title="view meeting"
                aria-label="view meeting"
                label="View Meeting"
                onClick={toggleViewMeetingDialog}
            >View Meeting Body</Link>
        },
    };
    columns.push(overflowCol);

    const onRenderDetailsHeader = (props, defaultRender) => {
        if (!props) {
            return null;
        }
        return (
            <Sticky 
            stickyPosition={StickyPositionType.Header} 
            isScrollSynced 
            stickyBackgroundColor="transparent">
                {defaultRender({
                    ...props
                })}
            </Sticky>
        );
    }

    const filterProperties = [
        { key: 0, text: "NotificationId", selector: "InputBox", value: [], placeholder: AppConstants.PlaceholderCommaSeparated, isList:true},
        { key: 1, text: "Status", selector: "ComboBox", value: [{ key: 0, text: "Queued" }, { key: 1, text: "Processing" },
            { key: 2, text: "Retrying" }, { key: 3, text: "Failed" }, { key: 4, text: "Sent" },], placeholder: AppConstants.PlaceholderSelectValue , isList:true
        },
        { key: 2, text: "SentOnStart", selector: "InputBox", value: [], placeholder: AppConstants.PlaceholderDateFormat, isList:false},
        { key: 3, text: "SentOnEnd", selector: "InputBox", value: [], placeholder: AppConstants.PlaceholderDateFormat, isList:false}
    ];

    const operatorItems = [{ key: 0, text: "==" }];

    const onApplyFilter = (obj) => {
        var diff = filter.filter(a =>  !obj.some(b =>  a.key === b.key && a.value === b.value)).concat(obj.filter(a => !filter.some(b => b.key === a.key && a.value === b.value)));
        if(diff.length === 0){
            return;
        }
        var appsFromFilter = obj.filter(a=> a.key === 4)?.[0]?.value;
        setApplicationName(appsFromFilter?.length > 0? appsFromFilter[0] : applications[0]);
        setFilter(obj);
        selectedPage.current = 1;
        fetchMeetingHistory(obj, null, defaultPageSize.current);
    }

    const onSelectAddFilter = (obj) => { }
    const onSelectFilter = (index, value) => { }
    const onFilterDismiss = (o) => { }
    const onResetFilter = () => {
        if(filter.length === 0){
            return;
        }

        selectedPage.current = 1;
        setFilter([]);
        fetchMeetingHistory(null, null, null);
    }
    const onActiveItemChanged = (i, indx, e) => {
        setActiveItem(i);
    }

    const toggleViewMeetingDialog = () => {
        setLoader(true);
        setMailBody(undefined);
        setMailDialog(!mailDialog);
        if (mailDialog === true) {
            getMeetingBody(activeItem?.application, activeItem?.notificationId).then((response) => {
                setMailBody(response?.data?.body);
                setLoader(false);
            }).catch((error) => {
                setMailBody(AppConstants.NotificationBodyLoadFailed);
                setLoader(false);
            })
        } else {
            setLoader(false);
        }
    }

    return (
        <ScrollablePane className={propertis.isNavCollapsed ? _Styles.scrollablePaneCollapsed : _Styles.scrollablePaneExpand}>
        <div>
            <Sticky >

                <Stack horizontal horizontalAlign="space-between">
                    <span style={{ textAlignLast: "left", paddingRight: 10 }}>
                        <ActionButton
                            iconProps={{ iconName: 'MailRepeat' }}
                            allowDisabledFocus
                            disabled={disableResend}
                            onClick={toggleHideDialog}>
                            Resend
                        </ActionButton>
                    </span>
                    <span style={{ textAlignLast: "right", paddingRight: 10 }}>
                        <MailHistoryFilter
                            filterItems={filterProps}
                            operatorItems={operatorItems}
                            onApplyFilter={onApplyFilter}
                            onResetFilter={onResetFilter}
                            onSelectAddFilter={onSelectAddFilter}
                            onSelectFilter={onSelectFilter}
                            onFilterDismiss={onFilterDismiss}
                        />
                    </span>
                </Stack>

            </Sticky>
            <div style={{ position: 'relative', height: "76vh" }}>
                <ToastContainer style= {{width: "162px"}}/>
                <ScrollablePane className={classNames.header} scrollbarVisibility={ScrollbarVisibility.auto}>
                        <ShimmeredDetailsList
                            setAllSelected='false'
                            selection={selection}
                            setKey='set'
                            enableShimmer={showShimmer}
                            columns={columns}
                            constrainMode={ConstrainMode.unconstrained}
                            items={meetingHistoryData ? meetingHistoryData : []}
                            isHeaderVisible={true}
                            checkButtonAriaLabel="checkButton"
                            ariaLabelForSelectAllCheckbox="selectallcheckbutton"
                            ariaLabel="checkButton"
                            onRenderDetailsHeader={onRenderDetailsHeader}
                            onActiveItemChanged={onActiveItemChanged}
                            selectionMode={SelectionMode.multiple}
                            onItemInvoked = {copyToClipboard}
                        />
                </ScrollablePane>
                <ResendModal
                    toggleHideDialog={toggleHideDialog}
                    hideDialog={hideDialog}
                    selectedItem={selectedItem}
                    application = {applicationName}
                    title="Resend Meeting Invites"
                    notificationType="Meet"
                />
                <ViewMailModal
                    toggleMailDialog={toggleViewMeetingDialog}
                    hideDialog={mailDialog}
                    selectedItem={activeItem}
                    mailBody={mailBody}
                    showLoader={loader}
                />
            </div>
            <Stack horizontal horizontalAlign="end">
                <Stack horizontal horizontalAlign="space-between" styles={{ root: { width: '50%' } }}>
                    <div style={{ marginLeft: -132 }}>
                        <CoherencePagination {...paginationProps}
                        />
                    </div>
                    {/* <CoherencePageSize {...paginationPageSizeProps} /> */}
                </Stack>
            </Stack>
        </div>
        </ScrollablePane>
    )
}
